local DisableInputTrigger = {}


DisableInputTrigger.name = "Time_Switch/DisableInputTrigger"

DisableInputTrigger.placements = {
    name = "Disable_Input",
    data = {
		PlayerInput = 1,
		Activate = 1,
		onlyOnce = true
    }
}

DisableInputTrigger.fieldOrder = {
	"x",
	"y",
	"width",
	"height",
	"PlayerInput",
	"onlyOnce",
	"Activate"
}

DisableInputTrigger.fieldInformation = { 
	PlayerInput = {
		fieldType = "integer",
		options = {
			["Disable"] = 1,
            ["Enable"] = 2,
		},
		editable = false
	},
	Activate = {
		fieldType = "integer",
        options = {
			["OnPlayerEnter"] = 1,
            ["OnPlayerLeave"] = 2,
            ["OnRoomLoad"] = 3,
		},
        editable = false
    }
}


return DisableInputTrigger