# TheCrushinator FitBit Integration

An app to allow assorted integration with FitBit

## Secrets

ClientId and ClientSecret for the FitBit account are both treated as secrets, to set these up...

1. Ensure you have secrets initialised

    ```bash
    dotnet user-secrets init --project TheCrushinator.FitBit.Web
    ```

1. Add the secrets

    ```bash
    dotnet user-secrets set "FitBit:ClientId" "xxx" --project TheCrushinator.FitBit.Web
    dotnet user-secrets set "FitBit:ClientSecret" "xxx" --project TheCrushinator.FitBit.Web
    ```

