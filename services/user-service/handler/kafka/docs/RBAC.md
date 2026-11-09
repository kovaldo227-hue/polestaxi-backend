# RBAC – Gateway

Проверка access-tokens: Gateway -> Auth ValidateToken (gRPC).
Дальше – роль из ответа. Downstream не обязан повторно проверять JWT.

Префикс `/v1/auth/*` (login, refresh, logout) и `POST /v1/users/register` – **без** токена.

| Префикс / метод | User | Driver | Analyst | Admin |
| --- | --- | --- | --- | --- |
| `POST /v1/users/register` | anonymous | anonymous | – | – |
| `/v1/users/me` | да | да | да | да |
| `POST /v1/users/{id}/roles` | нет | нет | нет | да |
| `/v1/drivers/me` | нет | да | нет | да |
| `POST /v1/orders` | да | нет | нет | да |
| `GET /v1/orders` (свои) | да | да (свои как водитель) | да (поиск) | да |
| `/v1/wallets/me` | да | да | нет | да |
| `/v1/analytics/*` | нет | нет | да | да |
| внутренние gRPC | не через Gateway | не через Gateway | – | – |

NGINX: `limit_req` + upstream на Gateway.
Публичная регистрация ставит только User или Driver (RegistrationAllowed в Shared).