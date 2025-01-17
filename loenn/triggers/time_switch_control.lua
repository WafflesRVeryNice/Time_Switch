local TimeSwitchControlTrigger = {}


TimeSwitchControlTrigger.name = "Time_Switch/TimeSwitchControlTrigger"

TimeSwitchControlTrigger.placements = {
    name = "Time_Switch_Control",
    data = {
        RoomNameFormat = 1,
		Activate = 1,
		Timelines = 1,
		onlyOnce = true
    }
}

TimeSwitchControlTrigger.fieldOrder = {
	"x",
	"y",
	"width",
	"height",
	"RoomNameFormat",
	"Activate",
	"Timelines",
	"onlyOnce"
}

TimeSwitchControlTrigger.fieldInformation = { 
    RoomNameFormat = {
        fieldType = "integer",
		options = {
            ["no change"] = 1,
            ["off"] = 2,
            ["simple"] = 3,
            ["clean"] = 4,
        },
		editable = false
    },
	Timelines = {
        fieldType = "integer",
		options = {
            ["no change"] = 1,
            ["automatic detection"] = 2,
            ["legacy / save to savefile"] = 3,
            ["legacy / save to session"] = 4,
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


return TimeSwitchControlTrigger