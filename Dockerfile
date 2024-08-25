FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

# Copy the application files and build them.
WORKDIR /build
COPY . .
RUN dotnet build Bouncer -c release -r linux-musl-x64 --self-contained -o /publish

# Switch to a container for runtime.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as runtime

# Prepare the runtime.
WORKDIR /app
COPY --from=build /publish .
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN ln -s Bouncer.dll app.dll
ENTRYPOINT ["dotnet", "/app/app.dll"]