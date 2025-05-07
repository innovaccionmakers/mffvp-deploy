# Use a minimal runtime image
FROM debian:bullseye-slim

# Create app directory
WORKDIR /app

# Copy everything from the publish output
COPY . .

# Make binary executable
RUN chmod +x MFFVP.Api

# Run the app
CMD ["./MFFVP.Api"]
