# Script para configurar GitHub MCP en VS Code
# Ejecuta este script después de crear tu GitHub Personal Access Token

Write-Host "🔧 Configuración de GitHub MCP para VS Code" -ForegroundColor Cyan
Write-Host ""
Write-Host "Este script configurará la variable de entorno GITHUB_TOKEN necesaria para el servidor MCP." -ForegroundColor Yellow
Write-Host ""

# Pedir el token de forma segura
$token = Read-Host "Pega tu GitHub Personal Access Token (no se mostrará en pantalla)" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($token)
$plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
[System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)

if ([string]::IsNullOrWhiteSpace($plainToken)) {
    Write-Host "❌ No se proporcionó ningún token. Abortando." -ForegroundColor Red
    exit 1
}

# Configurar la variable de entorno a nivel de usuario (persistente)
try {
    [System.Environment]::SetEnvironmentVariable('GITHUB_TOKEN', $plainToken, 'User')
    Write-Host "✅ Variable de entorno GITHUB_TOKEN configurada correctamente" -ForegroundColor Green
    
    # También configurar para la sesión actual
    $env:GITHUB_TOKEN = $plainToken
    Write-Host "✅ Token también disponible en esta sesión de PowerShell" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🎯 Próximos pasos:" -ForegroundColor Cyan
    Write-Host "1. Cierra COMPLETAMENTE VS Code (todas las ventanas)" -ForegroundColor White
    Write-Host "2. Abre VS Code de nuevo en este directorio: code ." -ForegroundColor White
    Write-Host "3. Abre el chat de Copilot y prueba el MCP de GitHub" -ForegroundColor White
    Write-Host ""
    Write-Host "Para probar, pregúntale a Copilot:" -ForegroundColor Yellow
    Write-Host '  "Lista los issues abiertos de este repositorio"' -ForegroundColor Gray
    
} catch {
    Write-Host "❌ Error al configurar la variable: $_" -ForegroundColor Red
    exit 1
}
