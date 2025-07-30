import pexpect
import json
import os

child = pexpect.spawn(f'dotnet run --project WeatherMcpServer', env=os.environ, timeout=60)

child.expect('Application started.')

request = {
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "get_weather_alerts",
        "arguments": {
            "city": "Moscow"
        }
    },
    "id": 1
}

child.sendline(json.dumps(request))

child.expect('"result":')

print(child.before.decode() + child.after.decode())
print(child.read().decode())

child.close()
