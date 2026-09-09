using FluentAssertions;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using MuhasibPro.Business.Contracts.UIServices.CommonServices.Events;
using MuhasibPro.Business.Services.CommonServices;

namespace MuhasibPro.Tests;

public class EventBusTests
{
    private sealed record TestOlayi(string Ad) : DomainEvent("Test", DateTime.UtcNow);
    private sealed record BaskaOlay(string Ad) : DomainEvent("Test", DateTime.UtcNow);

    /// <summary>Gerçek MessageService'in test ikizi (App katmanına dokunmadan).</summary>
    private sealed class SahteTasiyici : IMessageService
    {
        private readonly Dictionary<(object, string), Delegate> _kayitlar = new();
        public void Subscribe<TSender>(object target, Action<TSender, string, object> action) where TSender : class
            => Subscribe<TSender, object>(target, action);
        public void Subscribe<TSender, TArgs>(object target, Action<TSender, string, TArgs> action) where TSender : class
        {
            var anahtar = (target, typeof(TArgs).Name);
            if (_kayitlar.ContainsKey(anahtar))
                throw new ArgumentException("yinelenen abonelik");
            _kayitlar[anahtar] = action;
        }
        public void Unsubscribe(object target)
        {
            foreach (var k in _kayitlar.Keys.Where(k => ReferenceEquals(k.Item1, target)).ToList())
                _kayitlar.Remove(k);
        }
        public void Unsubscribe<TSender>(object target) where TSender : class => Unsubscribe(target);
        public void Unsubscribe<TSender, TArgs>(object target) where TSender : class => Unsubscribe(target);
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            foreach (var (hedef, ad, temsilci) in _kayitlar.Select(k => (k.Key.Item1, k.Key.Item2, k.Value)).ToList())
            {
                if (ReferenceEquals(hedef, sender) || ad != typeof(TArgs).Name)
                    continue;
                temsilci.DynamicInvoke(sender, message, args);
            }
        }
    }

    private static EventBus Kur() => new(new SahteTasiyici());

    [Fact]
    public void Publish_AboneOlayi_TipliIletir()
    {
        var bus = Kur();
        var hedef = new object();
        TestOlayi? alinan = null;
        bus.Subscribe<TestOlayi>(hedef, (_, e) => alinan = e);

        bus.Publish(this, new TestOlayi("merhaba"));

        alinan.Should().NotBeNull();
        alinan!.Ad.Should().Be("merhaba");
        alinan.Module.Should().Be("Test");
    }

    [Fact]
    public void Subscribe_IkinciKez_IdempotentAtmaz()
    {
        var bus = Kur();
        var hedef = new object();
        int sayac = 0;
        bus.Subscribe<TestOlayi>(hedef, (_, _) => sayac++);

        var eylem = () => bus.Subscribe<TestOlayi>(hedef, (_, _) => sayac++);
        eylem.Should().NotThrow();

        bus.Publish(this, new TestOlayi("x"));
        sayac.Should().Be(1);
    }

    [Fact]
    public void Unsubscribe_Sonrasi_Iletmez()
    {
        var bus = Kur();
        var hedef = new object();
        int sayac = 0;
        bus.Subscribe<TestOlayi>(hedef, (_, _) => sayac++);
        bus.Unsubscribe(hedef);

        bus.Publish(this, new TestOlayi("x"));

        sayac.Should().Be(0);
    }

    [Fact]
    public void FarkliOlayTipleri_BirbirineKarismaz()
    {
        var bus = Kur();
        var hedef = new object();
        int sayac = 0;
        bus.Subscribe<TestOlayi>(hedef, (_, _) => sayac++);

        bus.Publish(this, new BaskaOlay("y"));

        sayac.Should().Be(0);
    }
}
