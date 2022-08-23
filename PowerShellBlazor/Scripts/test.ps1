for ($i=1; $i -le 5; $i++) {
  Write-Progress "Loop $i - progress output (Write-Progress)"
  Write-Output "Normal output text (Write-Output)"
  Write-Warning "Here's some warning text (Write-Warning)"
  Write-Error "Oh no, here's some error text (Write-Error)"
  Start-Sleep -s 1
}