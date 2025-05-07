FROM debian:bullseye-slim

WORKDIR /app

COPY . .

# Install ICU
RUN apt-get update && apt-get install -y libicu67 && rm -rf /var/lib/apt/lists/*

RUN chmod +x MFFVP.Api

CMD ["./MFFVP.Api"]
