# GitHub login setup

1. Open https://github.com/settings/developers and create an **OAuth App**.
2. Set **Authorization callback URL** to `http://localhost:5253/signin-github`.
3. Put the app's Client ID and Client Secret in `backend/appsettings.json` under `Authentication:GitHub`, or set environment variables:
   - `Authentication__GitHub__ClientId`
   - `Authentication__GitHub__ClientSecret`
4. Start the ASP.NET backend on `http://localhost:5253` and the React frontend on `http://localhost:5173`.

The frontend button redirects to the backend OAuth endpoint. The backend handles GitHub's authorization-code callback and stores the signed-in session in an HTTP-only cookie. `/auth/me` reports the current login and `/auth/logout` signs out.

Do not commit a real Client Secret to source control. For deployment, configure HTTPS and set the OAuth callback URL and frontend URL to your deployed addresses.
