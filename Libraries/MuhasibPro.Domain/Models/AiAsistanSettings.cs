namespace MuhasibPro.Domain.Models
{
    /// <summary>
    /// Faz 6.92 AI Yardım Asistanı ayarları — tüm değerler modelden okunur.
    /// Kalıcılık: ILocalSettingsService, anahtar <see cref="SettingsKey"/> (makine geneli: model diske bir kez iner).
    /// </summary>
    public class AiAsistanSettings
    {
        public const string SettingsKey = "AiAsistanSettings";

        /// <summary>Foundry katalog model adı. P2 kararı (Oturum 291): `qwen2.5-0.5b` kalıcı varsayılan (S1 tam kilit).</summary>
        public const string VarsayilanModelAlias = "qwen2.5-0.5b";

        /// <summary>S2 deterministik üretim (Oturum 292): sohbet sıcaklığı. 0 = aynı soruya aynı cevap.</summary>
        public const float UretimSicakligi = 0f;

        /// <summary>S2 deterministik üretim (Oturum 292): sabit üretim tohumu (temperature ile birlikte tekrarlanabilirlik).</summary>
        public const int UretimSabitTohumu = 292291;

        /// <summary>Foundry katalog embedding model adı (Faz 6.93 hibrit RAG).</summary>
        public const string VarsayilanEmbeddingModelAlias = "qwen3-embedding-0.6b";

        /// <summary>Foundry katalog model adı. Kritik — değişimi indirme/disk etkisi yapar, yalnızca yönetici.</summary>
        [YoneticiAyari]
        public string ModelAlias { get; set; } = VarsayilanModelAlias;

        /// <summary>Foundry katalog embedding model adı. Kritik — değişimi yeniden indeksleme etkisi yapar, yalnızca yönetici.</summary>
        [YoneticiAyari]
        public string EmbeddingModelAlias { get; set; } = VarsayilanEmbeddingModelAlias;

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

        public string GetEmbeddingModelAlias() =>
            string.IsNullOrWhiteSpace(EmbeddingModelAlias) ? VarsayilanEmbeddingModelAlias : EmbeddingModelAlias.Trim();

        public int GetMaksGecmisTur() => Math.Clamp(MaksGecmisTur, 0, 20);

        public int GetEnFazlaMadde() => Math.Clamp(EnFazlaMadde, 1, 12);

        public int GetSoruZamanAsimiSn() => Math.Clamp(SoruZamanAsimiSn, 10, 300);
    }
}
