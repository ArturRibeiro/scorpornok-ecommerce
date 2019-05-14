Inicia o migration:
	add-migration InitialCreate -verbose

Criação:
	Add-Migration v1 -Context PainelIndicatorDataContext -Verbose

Atualização:
	Update-Database -Context PainelIndicatorDataContext -Verbose

Remoção:
	Remove-Migration -Context PainelIndicatorDataContext -Verbose