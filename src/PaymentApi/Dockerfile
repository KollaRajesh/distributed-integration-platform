FROM python:3.12-slim

WORKDIR /app

COPY pyproject.toml .
RUN pip install --no-cache-dir .

COPY payment_api ./payment_api
COPY .env.example .env

EXPOSE 8000
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:8000/health/live')"

ENTRYPOINT ["python", "-m", "uvicorn", "payment_api.main:app", "--host", "0.0.0.0", "--port", "8000"]
