param(
    [string]$BaseUrl = 'http://localhost:5020'
)

$ErrorActionPreference = 'Stop'

# Explicit local-only seed path for Basket sample data. Call this against the
# already-running Basket API in Development.

function Pause-BeforeExit {
    Write-Host ''
    Read-Host 'Press Enter to close'
}

try {
    $seedUrl = '{0}/internal/dev/seed' -f $BaseUrl.TrimEnd('/')

    Write-Host 'Running Basket dev data seed...'
    Write-Host ''
    Write-Host "POST $seedUrl"
    Write-Host ''

    $response = Invoke-RestMethod -Method Post -Uri $seedUrl

    if ($null -eq $response) {
        throw 'Seed endpoint returned no response.'
    }

    Write-Host ''
    Write-Host $response.message
}
catch {
    Write-Host ''
    Write-Host 'Basket dev data seed failed.' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Pause-BeforeExit
}
