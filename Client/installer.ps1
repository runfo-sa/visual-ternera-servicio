# Instalador para el Servicio controlador de etiquetas de Visual Ternera
# Autor: Agustin Marco <agustin.marco@runfo.com.ar>

$ServiceName = "VSTC"
$DisplayName = "Visual Ternera - Controlador de Etiquetas"
$FolderPath = "C:\soft\Visual Ternera\Service\"
$FilePath = "C:\soft\Visual Ternera\Service\Client.exe"

New-Item -ItemType Directory -Force -Path $FolderPath
Invoke-WebRequest http://rafatest.runfosa.local:8089/obtenercliente?key=ABC123 -OutFile $FilePath

$Service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($Service -eq $null)
{
	$params = @{
	  Name = $ServiceName
	  BinaryPathName = $FilePath
	  DisplayName = $DisplayName
	  Description = "Controla el estado de etiquetas en este equipo."
	}

	New-Service @params
	sc.exe config $ServiceName start= delayed-auto
	Start-Service -Name $ServiceName
}
else
{
	Get-Service -Name $ServiceName
	Restart-Service -Name $ServiceName
}
