@echo off
setlocal enabledelayedexpansion

:: Configuración
set "dotnet_version=net8.0"
set "solution_name=Products"
set "sln_file=MFFVP.sln"

:: Capas del módulo
set layers[0]=Application
set layers[1]=Domain
set layers[2]=Infrastructure
set layers[3]=Integrations
set layers[4]=Presentation
set layers[5]=test

:: Verificar solución
if not exist "%sln_file%" (
    echo Error: No se encontró el archivo %sln_file%
    pause
    exit /b 1
)

:: PRIMERA PASADA: Crear proyectos y agregarlos a la solución
for /l %%i in (0,1,5) do (
    set "layer_name=!layers[%%i]!"
    set "project_name=%solution_name%.!layer_name!"
    set "project_folder=src\Modules\%solution_name%\!layer_name!"

    echo.
    echo === Procesando capa !layer_name! ===

    :: Asegurar carpeta
    if not exist "!project_folder!" (
        echo Creando carpeta !project_folder!...
        mkdir "!project_folder!"
    ) else (
        echo Carpeta !project_folder! ya existe.
    )
    pushd "!project_folder!"

    if "!layer_name!"=="test" (
        :: Crear proyectos de test
        for %%t in (UnitTests IntegrationTests) do (
            set "test_dir=%%t"
            set "test_proj=%solution_name%.test.%%t"
            set "csproj_path=%%t\%solution_name%.test.%%t.csproj"
            if not exist "!csproj_path!" (
                echo Creando proyecto %%t...
                mkdir "%%t" 2>nul
                pushd "%%t"
                dotnet new xunit --framework %dotnet_version% --name "!test_proj!" --output .
                dotnet sln "..\..\..\..\..\%sln_file%" add "%solution_name%.test.%%t.csproj" --solution-folder "!project_folder!"
                popd
            ) else (
                echo Proyecto %%t ya existe.
            )
        )
    ) else (
        :: Crear proyecto de capa principal
        if not exist "!project_name!.csproj" (
            echo Creando proyecto !project_name!...
            dotnet new classlib --framework %dotnet_version% --name "!project_name!" --output .
            :: Eliminar archivo por defecto Class1.cs
            if exist "Class1.cs" del "Class1.cs"
            dotnet sln "..\..\..\..\%sln_file%" add "!project_name!.csproj" --solution-folder "!project_folder!"
        ) else (
            echo Proyecto !project_name! ya existe.
            dotnet sln "..\..\..\..\%sln_file%" list | findstr /i "!project_name!.csproj" >nul || (
                echo Agregando proyecto existente a la solución...
                dotnet sln "..\..\..\..\%sln_file%" add "!project_name!.csproj" --solution-folder "!project_folder!"
            )
        )
    )
    popd
)

:: SEGUNDA PASADA: Agregar referencias entre proyectos y Common

echo.
echo === Agregando referencias ===
for /l %%i in (0,1,5) do (
    set "layer_name=!layers[%%i]!"
    set "project_folder=src\Modules\%solution_name%\!layer_name!"
    pushd "!project_folder!"

    if not "!layer_name!"=="test" (
        echo - Agregando referencias Common SharedKernel...
        dotnet add reference "..\..\..\Common\Common.SharedKernel.Application\Common.SharedKernel.Application.csproj"
        dotnet add reference "..\..\..\Common\Common.SharedKernel.Domain\Common.SharedKernel.Domain.csproj"
        dotnet add reference "..\..\..\Common\Common.SharedKernel.Infrastructure\Common.SharedKernel.Infrastructure.csproj"
        dotnet add reference "..\..\..\Common\Common.SharedKernel.Presentation\Common.SharedKernel.Presentation.csproj"
    )

    if "!layer_name!"=="Application" (
        echo - Agregando referencias específicas a Domain e Integrations...
        dotnet add reference "..\Domain\%solution_name%.Domain.csproj"
        dotnet add reference "..\Integrations\%solution_name%.Integrations.csproj"
    ) else if "!layer_name!"=="Infrastructure" (
        echo - Agregando referencias específicas a Application, Domain y Presentation...
        dotnet add reference "..\Application\%solution_name%.Application.csproj"
        dotnet add reference "..\Domain\%solution_name%.Domain.csproj"
        dotnet add reference "..\Presentation\%solution_name%.Presentation.csproj"
    ) else if "!layer_name!"=="Integrations" (
        echo - Agregando referencia específica a Domain...
        dotnet add reference "..\Domain\%solution_name%.Domain.csproj"
    ) else if "!layer_name!"=="Presentation" (
        echo - Agregando referencia específica a Application...
        dotnet add reference "..\Application\%solution_name%.Application.csproj"
    ) else if "!layer_name!"=="test" (
        :: UnitTests
        echo - Agregando referencias de UnitTests...
        pushd "UnitTests"
        dotnet add reference "..\..\Application\%solution_name%.Application.csproj"
        dotnet add reference "..\..\Domain\%solution_name%.Domain.csproj"
        popd
        :: IntegrationTests
        echo - Agregando referencia de IntegrationTests...
        pushd "IntegrationTests"
        dotnet add reference "..\..\Infrastructure\%solution_name%.Infrastructure.csproj"
        popd
    )
    popd
)

echo.
echo Proceso completo: todos los proyectos creados y referencias aplicadas.
pause
exit /b
