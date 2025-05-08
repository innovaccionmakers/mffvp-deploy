FROM debian:bullseye-slim

RUN apt-get update && apt-get install -y \
    libicu67 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY . .

RUN chmod +x MFFVP.Api

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV PORT=8080
EXPOSE ${PORT}

CMD ["sh", "-c", "./MFFVP.Api --urls http://0.0.0.0:$PORT"]
