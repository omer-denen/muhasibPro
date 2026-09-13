# View Bağımlılık Haritası

> Üretim: Oturum 177 (52 XAML tarandı). Alt satır = içerir/açar. Tipler: Page / UserControl / ContentDialog.

## Bağımlılık ağaçları (kök sayfalar)

- `DatabaseSettingsView` *(Page)*

- `DetailsWindow` *(Window)*

- `ExtendedSplash` *(Page)*
  - `SplashStatusControl` *(UserControl)*

- `FirmaShellView` *(Page)*
  - `FirmalarListControl` *(UserControl)*
  - `MaliDonemlerListControl` *(UserControl)*
    - `SagaPipelineDialog` *(ContentDialog)*
  - `UserInfoControl` *(UserControl)*

- `FirmaView` *(Page)*
  - `FirmaDetails` *(UserControl)*
    - `FirmaCard` *(UserControl)*

- `FirmalarView` *(Page)*
  - `FirmaMaliDonemler` *(UserControl)*
  - `FirmalarDetails` *(UserControl)*
    - `FirmalarCard` *(UserControl)*
  - `FirmalarList` *(UserControl)*

- `LoginView` *(Page)*
  - `NamePasswordControl` *(UserControl)*
    - `AnimatedInfoBorder` *(UserControl)*
  - `QuickLoginPanel` *(UserControl)*

- `MainShellView` *(Page)*
  - `NavSidebarControl` *(UserControl)*

- `MaliDonemView` *(Page)*
  - `MaliDonemDetails` *(UserControl)*

- `MaliDonemYonetimView` *(Page)*
  - `BilinmeyenPanel` *(UserControl)*
  - `DeleteGuardDialog` *(ContentDialog)*
  - `DerinAnalizPanel` *(UserControl)*
  - `DonemIslemKartlariPanel` *(UserControl)*
  - `DonemOzetCard` *(UserControl)*
  - `DonemParametrePanel` *(UserControl)*
  - `DonemSecimListesi` *(UserControl)*
  - `DonemYedeklerPanel` *(UserControl)*
  - `TopluIslemlerPanel` *(UserControl)*
  - `YonetimAyarlarDialog` *(ContentDialog)*
    - `AyarGenelPanel` *(UserControl)*
    - `AyarListeGorunumPanel` *(UserControl)*
    - `AyarSaklamaPanel` *(UserControl)*

- `QuickSistemDbDiagDialog` *(ContentDialog)*

- `RestoreVerifyDialog` *(ContentDialog)*

- `ShellView` *(Page)*
  - `ShellStatusBar` *(UserControl)*

- `SistemKurulumView` *(Page)*
  - `DatabaseInfoPanel` *(UserControl)*
  - `KurulumKayitPanel` *(UserControl)*
  - `SystemTestsPanel` *(UserControl)*

- `TenantDatabaseUpdateDialog` *(ContentDialog)*

- `TenantDatabaseUpdateView` *(Page)*

- `TransferDialog` *(ContentDialog)*

- `UpdateView` *(Page)*

- `YeniDonemDialog` *(ContentDialog)*

## Yükleme sırası (bağımlılık önce)

```
 1. AnimatedInfoBorder,
 2. DetailsWindow,
 3. FirmaCard,
 4. FirmaDetails,
 5. FirmaView,
 6. FirmaMaliDonemler,
 7. FirmalarCard,
 8. FirmalarDetails,
 9. FirmalarList,
10. FirmalarView,
11. NamePasswordControl,
12. QuickLoginPanel,
13. LoginView,
14. NavSidebarControl,
15. MainShellView,
16. MaliDonemDetails,
17. MaliDonemView,
18. BilinmeyenPanel,
19. DeleteGuardDialog,
20. DerinAnalizPanel,
21. DonemIslemKartlariPanel,
22. DonemOzetCard,
23. DonemParametrePanel,
24. DonemSecimListesi,
25. DonemYedeklerPanel,
26. TopluIslemlerPanel,
27. AyarGenelPanel,
28. AyarListeGorunumPanel,
29. AyarSaklamaPanel,
30. YonetimAyarlarDialog,
31. MaliDonemYonetimView,
32. DatabaseSettingsView,
33. UpdateView,
34. RestoreVerifyDialog,
35. TenantDatabaseUpdateDialog,
36. TransferDialog,
37. YeniDonemDialog,
38. FirmalarListControl,
39. SagaPipelineDialog,
40. MaliDonemlerListControl,
41. UserInfoControl,
42. FirmaShellView,
43. ShellStatusBar,
44. ShellView,
45. TenantDatabaseUpdateView,
46. SplashStatusControl,
47. ExtendedSplash,
48. DatabaseInfoPanel,
49. KurulumKayitPanel,
50. QuickSistemDbDiagDialog,
51. SystemTestsPanel,
52. SistemKurulumView
```
