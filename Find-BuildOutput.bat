@echo off
REM Diagnostic script to find SmartERP build output

echo.
echo ================================
echo SmartERP Build Output Locator
echo ================================
echo.

REM Check common locations
echo Checking common build locations...
echo.

REM Check bin\Release
if exist "SmartERP.UI\bin\Release" (
    echo [FOUND] SmartERP.UI\bin\Release
    dir "SmartERP.UI\bin\Release" /s /b | head -10
) else (
    echo [NOT FOUND] SmartERP.UI\bin\Release
)

echo.

REM Check bin\Release\net10.0-windows
if exist "SmartERP.UI\bin\Release\net10.0-windows" (
    echo [FOUND] SmartERP.UI\bin\Release\net10.0-windows
    dir "SmartERP.UI\bin\Release\net10.0-windows\" /b
) else (
    echo [NOT FOUND] SmartERP.UI\bin\Release\net10.0-windows
)

echo.

REM Check bin\Release\net10.0
if exist "SmartERP.UI\bin\Release\net10.0" (
    echo [FOUND] SmartERP.UI\bin\Release\net10.0
    dir "SmartERP.UI\bin\Release\net10.0\" /b
) else (
    echo [NOT FOUND] SmartERP.UI\bin\Release\net10.0
)

echo.
echo ================================
echo Search complete
echo ================================
echo.
