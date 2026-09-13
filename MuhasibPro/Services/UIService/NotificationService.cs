using CommunityToolkit.WinUI.Notifications;
using MuhasibPro.Business.Contracts.UIServices.CommonServices;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.UI.Notifications;

namespace MuhasibPro.Services.UIService;

public class NotificationService : INotificationService
{
    public const string Aumid = "MuhasibProSoft.MuhasibPro";
    private const string DisplayName = "MuhasibPro";

    private static bool _initialized;

    /// <summary>
    /// Unpackaged uygulama için AUMID registry kaydı + Start Menu shortcut'ı oluşturur.
    /// Asla başlatmayı kırmaz (try/catch) — toast kullanılamazsa sessizce devam edilir.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            EnsureAumidRegistered();
            EnsureStartMenuShortcut();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NotificationService init hatası: {ex.Message}");
        }
    }

    public void Show(string title, string message, NotificationType type = NotificationType.Info)
        => ShowTagged(title, message, type, string.Empty, string.Empty);

    public void ShowTagged(string title, string message, NotificationType type, string tag, string group)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            if (type != NotificationType.Info)
                builder.AddAttributionText(NotificationGroups.Etiket(type));

            if (!string.IsNullOrEmpty(group))
                builder.AddHeader(group, $"MuhasibPro • {NotificationGroups.Baslik(group)}", string.Empty);

            var content = builder.GetToastContent();

            var toast = new ToastNotification(content.GetXml());
            if (!string.IsNullOrEmpty(tag))
                toast.Tag = tag;
            if (!string.IsNullOrEmpty(group))
                toast.Group = group;
            ToastNotificationManager.CreateToastNotifier(Aumid).Show(toast);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"NotificationService.ShowTagged hatası: {ex.Message}");
        }
    }

    private static void EnsureAumidRegistered()
    {
        using var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{Aumid}");
        key?.SetValue("DisplayName", DisplayName);
        var exePath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(exePath))
            key?.SetValue("IconUri", exePath);
    }

    private static void EnsureStartMenuShortcut()
    {
        var lnkPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft", "Windows", "Start Menu", "Programs", $"{DisplayName}.lnk");
        if (File.Exists(lnkPath)) return;

        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return;

        dynamic? shell = null;
        dynamic? lnk = null;
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
#pragma warning disable IL2072 // WScript.Shell COM aktivasyonu trim-güvenli değil; uygulama windows-only unpacked çalışır
            shell = Activator.CreateInstance(shellType);
#pragma warning restore IL2072
            if (shell == null) return;
            lnk = shell.CreateShortcut(lnkPath);
            lnk.TargetPath = exePath;
            lnk.WorkingDirectory = Path.GetDirectoryName(exePath);
            lnk.IconLocation = $"{exePath},0";
            lnk.Description = DisplayName;
            lnk.Save();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Shortcut oluşturulamadı: {ex.Message}");
            return;
        }
        finally
        {
            if (lnk != null) Marshal.FinalReleaseComObject(lnk);
            if (shell != null) Marshal.FinalReleaseComObject(shell);
        }

        SetShortcutAppUserModelId(lnkPath);
    }

    private static void SetShortcutAppUserModelId(string lnkPath)
    {
        try
        {
            IntPtr store = IntPtr.Zero;
            try
            {
                var key = new PROPERTYKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5); // PKEY_AppUserModel_ID
                var iid = IID_IPropertyStore;
                int hr = SHGetPropertyStoreFromParsingName(lnkPath, IntPtr.Zero, 2 /*GPS_READWRITE*/, ref iid, out store);
                if (hr != 0 || store == IntPtr.Zero) return;
                if (Marshal.GetObjectForIUnknown(store) is IPropertyStore props)
                {
                    var pv = new PROPVARIANT
                    {
                        vt = (ushort)VarEnum.VT_LPWSTR,
                        pointerValue = Marshal.StringToCoTaskMemUni(Aumid)
                    };
                    try
                    {
                        props.SetValue(ref key, ref pv);
                        props.Commit();
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pv.pointerValue);
                        Marshal.ReleaseComObject(props);
                    }
                }
            }
            finally
            {
                if (store != IntPtr.Zero) Marshal.Release(store);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AppUserModelID set edilemedi: {ex.Message}");
        }
    }

    private static readonly Guid IID_IPropertyStore = new("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99");

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHGetPropertyStoreFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
        IntPtr pbc,
        int flags,
        ref Guid riid,
        out IntPtr ppv);

    [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        int GetCount(out uint cProps);
        int GetAt(uint iProp, out PROPERTYKEY pkey);
        int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;

        public PROPERTYKEY(Guid formatId, uint propertyId)
        {
            fmtid = formatId;
            pid = propertyId;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public IntPtr pointerValue;
    }
}
