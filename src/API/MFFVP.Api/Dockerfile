FROM debian:bullseye-slim

# Install required libraries
RUN apt-get update && apt-get install -y \
    libicu67 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY . .

RUN chmod +x MFFVP.Api

# Globalization fallback (optional)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Railway provides the PORT environment variable
ENV PORT=8080

EXPOSE ${PORT}

# CMD must resolve PORT at runtime
CMD ["sh", "-c", "./MFFVP.Api --urls http://0.0.0.0:$PORT"]
