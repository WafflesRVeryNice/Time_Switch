local TimeSwitchControlTrigger = {}

TimeSwitchControlTrigger.name = "Time_Switch/TimeSwitchControlTrigger"
TimeSwitchControlTrigger.placements = {
    name = "Time_Switch_Control",
    data = {
        RoomNameFormat = 1,
		onlyOnce = true,
		coversScreen = false,
		AutomaticFormatDetection = 1,
		legacyTimelines = false
    }
}
TimeSwitchControlTrigger.fieldOrder = {
	"x",
	"y",
	"width",
	"height",
	"RoomNameFormat",
	"coversScreen",
	"onlyOnce",
	"AutomaticFormatDetection",
	"legacyTimelines"
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
	AutomaticFormatDetection = {
        fieldType = "integer",
		options = {
            ["no change"] = 1,
            ["off"] = 2,
            ["lightweight"] = 3,
            ["always"] = 4,
        },
		editable = false
    }
}

return TimeSwitchControlTrigger