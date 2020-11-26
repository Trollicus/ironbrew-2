local jsonString = [[
{
	"message": "success",
	"info": {
		"points": 120,
		"isLeader": true,
		"user": {
			"id": 12345,
			"name": "JohnDoe"
		},
		"past_scores": [50, 42, 95],
		"best_friend": null
	}
}
]]

print(game:GetService("HttpService"):JSONDecode(jsonString).message)