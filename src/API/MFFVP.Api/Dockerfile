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

CMD ["./MFFVP.Api"]
