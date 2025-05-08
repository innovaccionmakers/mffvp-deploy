FROM debian:bullseye-slim

# Install required system packages
RUN apt-get update && apt-get install -y \
    libicu67 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY . .

RUN chmod +x MFFVP.Api

# Optional: run in invariant globalization mode
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Dynamically bind to the port provided by Railway
ENV PORT=8080

EXPOSE ${PORT}

CMD ["sh", "-c", "./MFFVP.Api --urls http://0.0.0.0:$PORT"]
