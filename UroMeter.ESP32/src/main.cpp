#include <Arduino.h>
#include <PubSubClient.h>
#include <WiFiManager.h>
#include <WiFiClientSecure.h>

//---- WiFi settings
const char *WIFI_SSID = "HA 2";
const char *WIFI_PASSWORD = "conpassheo";

//---- HiveMQ Cloud Broker settings
const char *MQTT_BROKER = "";
const int MQTT_PORT = 8883;
const char *MQTT_USERNAME = "";
const char *MQTT_PASSWORD = "";

//---- MQTT Topic
const char *TOPIC_REGISTER = "register";

WiFiManager wm;
WiFiClientSecure espClient;
PubSubClient client(espClient);

// HiveMQ Cloud Let's Encrypt CA certificate (hardcoded)
static const char *root_ca PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIFazCCA1OgAwIBAgIRAIIQz7DSQONZRGPgu2OCiwAwDQYJKoZIhvcNAQELBQAw
TzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2Vh
cmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMTUwNjA0MTEwNDM4
WhcNMzUwNjA0MTEwNDM4WjBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJu
ZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBY
MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAK3oJHP0FDfzm54rVygc
h77ct984kIxuPOZXoHj3dcKi/vVqbvYATyjb3miGbESTtrFj/RQSa78f0uoxmyF+
0TM8ukj13Xnfs7j/EvEhmkvBioZxaUpmZmyPfjxwv60pIgbz5MDmgK7iS4+3mX6U
A5/TR5d8mUgjU+g4rk8Kb4Mu0UlXjIB0ttov0DiNewNwIRt18jA8+o+u3dpjq+sW
T8KOEUt+zwvo/7V3LvSye0rgTBIlDHCNAymg4VMk7BPZ7hm/ELNKjD+Jo2FR3qyH
B5T0Y3HsLuJvW5iB4YlcNHlsdu87kGJ55tukmi8mxdAQ4Q7e2RCOFvu396j3x+UC
B5iPNgiV5+I3lg02dZ77DnKxHZu8A/lJBdiB3QW0KtZB6awBdpUKD9jf1b0SHzUv
KBds0pjBqAlkd25HN7rOrFleaJ1/ctaJxQZBKT5ZPt0m9STJEadao0xAH0ahmbWn
OlFuhjuefXKnEgV4We0+UXgVCwOPjdAvBbI+e0ocS3MFEvzG6uBQE3xDk3SzynTn
jh8BCNAw1FtxNrQHusEwMFxIt4I7mKZ9YIqioymCzLq9gwQbooMDQaHWBfEbwrbw
qHyGO0aoSCqI3Haadr8faqU9GY/rOPNk3sgrDQoo//fb4hVC1CLQJ13hef4Y53CI
rU7m2Ys6xt0nUW7/vGT1M0NPAgMBAAGjQjBAMA4GA1UdDwEB/wQEAwIBBjAPBgNV
HRMBAf8EBTADAQH/MB0GA1UdDgQWBBR5tFnme7bl5AFzgAiIyBpY9umbbjANBgkq
hkiG9w0BAQsFAAOCAgEAVR9YqbyyqFDQDLHYGmkgJykIrGF1XIpu+ILlaS/V9lZL
ubhzEFnTIZd+50xx+7LSYK05qAvqFyFWhfFQDlnrzuBZ6brJFe+GnY+EgPbk6ZGQ
3BebYhtF8GaV0nxvwuo77x/Py9auJ/GpsMiu/X1+mvoiBOv/2X/qkSsisRcOj/KK
NFtY2PwByVS5uCbMiogziUwthDyC3+6WVwW6LLv3xLfHTjuCvjHIInNzktHCgKQ5
ORAzI4JMPJ+GslWYHb4phowim57iaztXOoJwTdwJx4nLCgdNbOhdjsnvzqvHu7Ur
TkXWStAmzOVyyghqpZXjFaH3pO3JLF+l+/+sKAIuvtd7u+Nxe5AW0wdeRlN8NwdC
jNPElpzVmbUq4JUagEiuTDkHzsxHpFKVK7q4+63SM1N95R1NbdWhscdCb+ZAJzVc
oyi3B43njTOQ5yOf+1CceWxG1bQVs5ZufpsMljq4Ui0/1lvh+wjChP4kqKOJ2qxq
4RgqsahDYVvTH9w7jXbyLeiNdd8XM2w9U/t7y0Ff/9yi0GE44Za4rF2LN9d11TPA
mRGunUHBcnWEvgJBQl9nJEiU0Zsnvgc/ubhPgXRR4Xq37Z0j4r7g1SgEEzwxA57d
emyPxgcYxn/eR44/KJ4EBs+lVDR3veyJm+kXQ99b21/+jh5Xos1AnX5iItreGCc=
-----END CERTIFICATE-----
)EOF";

void publishMessage(const char *topic, String payload, boolean retained);
void callback(char *topic, byte *payload, unsigned int length);

// put function declarations here:
void setupWifi(const char *ssid, const char *password);
void reconnectMQTT(const char *username, const char *password);

void setup()
{
  Serial.begin(115200);

  setupWifi(WIFI_SSID, WIFI_PASSWORD);

  espClient.setCACert(root_ca);
  client.setServer(MQTT_BROKER, MQTT_PORT);
  client.setCallback(callback);
}

void loop()
{
  if (!client.connected())
  {
    reconnectMQTT(MQTT_USERNAME, MQTT_PASSWORD);
  }

  client.loop();
  delay(10);
}

void publishMessage(const char *topic, String payload, boolean retained)
{
  if (client.publish(topic, payload.c_str(), true))
  {
    Serial.printf("*Message published with Topic \"%s\", Payload \"%s\"\n", topic, payload);
  }
}

void callback(char *topic, byte *payload, unsigned int length)
{
  String message = "";
  for (int i = 0; i < length; i++)
  {
    message += (char)payload[i];
  }

  Serial.printf("*Message received with Topic \"%s\", Length %i, Message \"%s\"\n", topic, length, message);
}

void setupWifi(const char *ssid, const char *password)
{
  WiFi.mode(WIFI_STA);
  wm.setTimeout(180);

  bool res = wm.autoConnect("ESP AP");

  if (!res)
  {
    Serial.println("Failed to connect");
  }

  Serial.printf("*WiFi connected with IP %s\n", WiFi.localIP().toString());
}

void reconnectMQTT(const char *username, const char *password)
{
  while (!client.connected())
  {
    Serial.print("*MQTT connection with ClientId ");

    String clientId = "ESP32-";
    clientId += String(random(0xffff), HEX);

    Serial.print(clientId);

    if (!client.connect(clientId.c_str(), username, password))
    {
      Serial.printf(": FAILED with State %i, Try again!\n", client.state());
      delay(2000);
    }
  }

  Serial.println(": CONNECTED");

  publishMessage(TOPIC_REGISTER, "Hello from ESP32", false);
  client.subscribe("backend");
}
