# Chess Variants Training

https://chessvariants.training/ is a website where you can improve your chess variant skills.

## Code setup

The solution file ChessVariantsTraining.sln can be opened with Visual Studio.

In the src/ChessVariantsTraining directory, you have to add a config-secret.json file:

```
{
  "Email": {
    "RequireEmailVerification": false,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "Password": "",
    "SenderName": "",
    "FromAddress": ""
  },
  "RecaptchaKey": ""
}
```

ChessVariantsTraining requires a glicko2-csharp dependency. You have to create a local NuGet package for https://github.com/MaartenStaa/glicko2-csharp and add it to the project.