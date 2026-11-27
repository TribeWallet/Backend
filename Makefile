# Atalhos do backend. Pré-requisitos: .env configurado, PostgreSQL e Mailpit rodando.
.PHONY: run test clean-tests

# O TribeWallet.csproj enxerga arquivos de Tests/bin e Tests/obj e o build quebra se eles existirem.
clean-tests:
	rm -rf Tests/bin Tests/obj

run: clean-tests
	dotnet run --launch-profile http

# Suíte completa. Usa o banco tribewallet_test (ou TRIBEWALLET_TEST_DB), nunca o de desenvolvimento.
test: clean-tests
	dotnet test Tests/TribeWallet.Tests.csproj
