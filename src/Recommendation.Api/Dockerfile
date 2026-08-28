FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/Recommendation.Api/Recommendation.Api.csproj ./src/Recommendation.Api/
WORKDIR /src/src/Recommendation.Api
RUN dotnet restore "Recommendation.Api.csproj"

# copy everything else and publish
COPY . /src
RUN dotnet publish "Recommendation.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Recommendation.Api.dll"]
