namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Faz 6.92 AI Yardım Asistanı ayarları — tüm değerler modelden okunur.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/> (makine geneli: model diske bir kez iner).
    /// </summary>
    public class AiAsistanSettings
    {
        public const string SettingsKey = "AiAsistanSettings";

        /// <summary>Foundry katalog model adı. Geçici varsayılan (doküman örneği); Türkçe final seçimi Windows canlı testte.</summary>
        public const string VarsayilanModelAlias = "qwen2.5-0.5b";

        /// <summary>Foundry katalog model adı. Kritik — değişimi indirme/disk etkisi yapar, yalnızca yönetici.</summary>
        [YoneticiAyari]
        public string ModelAlias { get; set; } = VarsayilanModelAlias;

        /// <summary>Asistan etkin mi? Kapalıysa giriş noktası pasif görünür.</summary>
        public bool EtkinMi { get; set; } = true;

        /// <summary>Soruya eklenen konuşma geçmişi (tur sayısı, kullanıcı+asistan çifti). 0-20 arası, varsayılan 4.</summary>
        public int MaksGecmisTur { get; set; } = 4;

        /// <summary>Prompt'a eklenen en fazla yardım maddesi. 1-12 arası, varsayılan 6.</summary>
        public int EnFazlaMadde { get; set; } = 6;

        /// <summary>Tek soru zaman aşımı (sn). 10-300 arası, varsayılan 60.</summary>
        public int SoruZamanAsimiSn { get; set; } = 60;

        public string GetModelAlias() =>
            string.IsNullOrWhiteSpace(ModelAlias) ? VarsayilanModelAlias : ModelAlias.Trim();

        public int GetMaksGecmisTur() => Math.Clamp(MaksGecmisTur, 0, 20);

        public int GetEnFazlaMadde() => Math.Clamp(EnFazlaMadde, 1, 12);

        public int GetSoruZamanAsimiSn() => Math.Clamp(SoruZamanAsimiSn, 10, 300);
    }
}
