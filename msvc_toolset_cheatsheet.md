# Visual Studio, MSVC Toolset, and Compiler Version Cheat Sheet

| Visual Studio Version | VS Version | MSVC Toolset Name | MSVC Compiler Version | Year Released | Notes                                           |
|----------------------|------------|-------------------|----------------------|--------------|-------------------------------------------------|
| Visual Studio 2015   | 14.0       | v140              | 14.00.xxxxx          | 2015         |                                                 |
| Visual Studio 2017   | 15.0-15.9  | v141              | 14.1x.xxxxx          | 2017-2019    | 15.0 = 14.10, 15.9 = 14.16                      |
| Visual Studio 2019   | 16.0-16.11 | v142              | 14.2x.xxxxx          | 2019-2022    | 16.0 = 14.20, 16.11 = 14.29                     |
| Visual Studio 2022   | 17.0-17.9  | v143              | 14.3x.xxxxx          | 2021-2024    | 17.0 = 14.30, 17.9 ≈ 14.39                      |
| Visual Studio 2022   | 17.10-17.x | v143              | 14.4x.xxxxx          | 2024+        | 17.10 = 14.40, still officially v143 toolset     |
| Visual Studio 2026   | 18.0+      | v145              | 14.50.xxxxx+         | 2026+        | v145 is the next major toolset after v143        |

## Details

- **There is NO v144 toolset**: After v143, the next major toolset is v145 (with Visual Studio 2026 and MSVC 14.50). The minor 14.4x versions in VS 2022 (17.10+) still use the v143 toolset and are not v144.
- **Toolset Name (`v141`, `v142`, etc.)**: MSBuild Platform Toolset used in `vcxproj` files, selects the matching compiler and libraries.
- **MSVC Compiler Version**: The version output by `cl.exe` when invoked as `/?` or in build logs.
- **VS Version Numbers**: Internal versioning, not the year (e.g., VS 2017 is 15.x, VS 2019 is 16.x, VS 2022 is 17.x).

## Example Toolset Mapping

| Toolset  | Visual Studio | Compiler Version Example |
|----------|---------------|-------------------------|
| v140     | 2015 (14.x)   | 14.00                   |
| v141     | 2017 (15.x)   | 14.16                   |
| v142     | 2019 (16.x)   | 14.29                   |
| v143     | 2022 (17.x)   | 14.30 – 14.49+          |
| v145     | 2026 (18.x)   | 14.50+                  |

## Notes

- Toolsets (`v14X`) are often installable side-by-side via Visual Studio Installer, allowing building older code in newer environments.
- Binary compatibility is maintained between v140, v141, v142, v143, and v145, but linking must be done with the newest version present in your set of binaries.
- When maintaining legacy code, specifying the correct toolset can be critical for binary compatibility.

## References

- [MSVC version numbers](https://learn.microsoft.com/en-us/cpp/overview-of-building-cpp-programs-on-windows?view=msvc-170#msvc-version-numbers)
- [Visual Studio Release History](https://learn.microsoft.com/en-us/visualstudio/releases/history)
- [MSVC Toolset Minor Version Number 14.40 in VS 2022 v17.10](https://devblogs.microsoft.com/cppblog/msvc-toolset-minor-version-number-14-40-in-vs-2022-v17-10/)
- [Upgrading C++ Projects to Visual Studio 2026](https://devblogs.microsoft.com/cppblog/upgrading-c-projects-to-visual-studio-2026/)
- [What's new for MSVC Build Tools](https://learn.microsoft.com/en-us/cpp/overview/what-s-new-for-msvc?view=msvc-170)
- [C++ binary compatibility 2015-2026](https://learn.microsoft.com/en-us/cpp/porting/binary-compat-2015-2017?view=msvc-170)