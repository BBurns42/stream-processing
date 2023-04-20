# Stream Processing

## Description
Stream Processing is a Coding challenge to process continuous and burstable data. This sample uses the Reddit API live threads to connect to a source of continuous data. Messages are then to be processed and to provide metrics to the user by analyzing each message.

This is achieved by using dedicated services for connecting to the Reddit live services or using the Mock service to generate messages into the RabbitMQ Queue for processing. This allows for multiple services to scale horizontally to add messages into the Queue. Similarly, a dedicated service is set to process messages from the Queue and to collect and measure metrics. This decoupled setup allows for potentially more messages to be published than consumed using RabbitMQ as a buffer. In this event, more consumers can be added to horizontally scale the system to keep up with demand and scale back down as needed.

![Service Diagram](./service%20diagram.png)

To accomodate the potential multiple sources of data, each Message Consumer service independently collects metrics using System Diagnostics. These in turn are broadcasted to an Open Telemetry service for collation and can be scraped by Prometheus. This in turn is used by Grafana for dashboards.

## Getting Started

### Reddit Live Thread

Query Reddit for a Live Thread Socket such as:

``` curl
curl --location 'https://www.reddit.com/live/ta535s1hq2je/about.json'
```

Copy the `data::websocket_url` into docker-compose.override.yml and replace the value `REDDIT_WS`.


### Build and Run Docker Compose script

To start up all the services, run:

```
docker-compose up -d
```
