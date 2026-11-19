# Delivery note web application

Angular front end for the delivery-note service. Requires Node.js 24 and npm 11.

Run every command from this `web/` directory.

| Task                                          | Command                |
| --------------------------------------------- | ---------------------- |
| Install dependencies                          | `npm ci`               |
| Run the dev server at `http://localhost:4200` | `npm start`            |
| Run the tests once                            | `npm test`             |
| Lint                                          | `npm run lint`         |
| Format all files                              | `npm run format`       |
| Check formatting                              | `npm run format:check` |
| Build for production into `dist/web/`         | `npm run build`        |

## Backend

The application calls the backend through the relative path `/api`. During
development, `npm start` proxies `/api` requests to `http://localhost:5211`
(see `proxy.conf.json`). Start the backend with its `http` launch profile first,
for example from `src/WebApp`:

```bash
dotnet run --launch-profile http
```
