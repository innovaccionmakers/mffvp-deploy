FROM debian:bullseye-slim

RUN apt-get update && apt-get install -y libicu67 curl ca-certificates && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
RUN chmod +x MFFVP.Api

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV PORT=8080
EXPOSE ${PORT}

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:$PORT/ping || exit 1

ENTRYPOINT ["sh", "-c", "./MFFVP.Api --urls http://0.0.0.0:$PORT"]
