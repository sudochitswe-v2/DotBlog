# DotBlog

> #### Configure User Secrets On Visual Studio

- Right click on project
- Select "Manage User Secret"
- "secrets.json" file should open in vs editor.
> #### Configure User Secrets On JetBrains Rider
- Right click on project
- Tools > .NET User Secrets
- Following is sample snippet for project

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<connection_string>"
  },
  "JwtSettings": {
    "SecretKey": "<key>",
    "Issuer": "<issuer>",
    "Audience": "<audience>",
    "RefreshTokenExpirationDays": <no of days>
  }
}
```
