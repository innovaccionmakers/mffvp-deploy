FROM debian:bullseye-slim

# Install required system packages
RUN apt-get update && apt-get install -y \
    libicu67 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY . .

RUN chmod +x MFFVP.Api

# Expose the port your app listens on (very important for Railway)
EXPOSE 8080

# Set environment flag for globalization
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Start your app, binding explicitly to 0.0.0.0:8080
CMD ["./MFFVP.Api", "--urls", "http://0.0.0.0:8080"]
