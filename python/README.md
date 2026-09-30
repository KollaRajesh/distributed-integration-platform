# Python service foundation

`foundation` contains the typed configuration and authentication dependency shared by the
Payment and Vendor FastAPI services. The service modules expose only health and foundation
routes until their domain work begins.

Run checks from this directory with `python -m pytest` and `ruff check .`.
