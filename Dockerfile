# Stage 1: Build stage using Mono
FROM mono:latest AS build
WORKDIR /src

# Copy source files
COPY . .

# Build using MSBuild (Mono's compiler supports legacy .NET Framework)
RUN msbuild /p:Configuration=Release KMSEmulator.NET/KMSEmulator.NET.csproj

# Stage 2: Runtime stage
FROM mono:slim AS final
WORKDIR /app

# Copy built binaries from the build stage
COPY --from=build /src/KMSEmulator.NET/bin/Release/ .

EXPOSE 1688

# Run with Mono
ENTRYPOINT ["mono", "KMSEmulator.NET.exe"]
