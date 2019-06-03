REM - Invoke from within a developer command prompt

msbuild PCHost\SimpleMonitor\SimpleMonitor.sln /t:Rebuild /p:Configuration=Release || exit /b !ERRORLEVEL!
pushd SpectrumHost
..\..\PasmoNext -1 --equ USEIPPLACEHOLDER=1 --alocal --bin monitor.asm monitor.bin || exit /b !ERRORLEVEL!
..\..\PasmoNext -1 --alocal --tapbas loader.asm monitor.tap || exit /b !ERRORLEVEL!
popd

rmdir /S/Q Release
mkdir Release\Monitor

copy PCHost\SimpleMonitor\bin\Release\*.dll Release\Monitor\ || exit /b !ERRORLEVEL!
copy PCHost\SimpleMonitor\bin\Release\SimpleMonitor.exe Release\Monitor\Monitor.exe || exit /b !ERRORLEVEL!

copy SpectrumHost\monitor.tap Release\Monitor\monitor.dpl || exit /b !ERRORLEVEL!
copy Docs\README.TXT Release\ || exit /b !ERRORLEVEL!
