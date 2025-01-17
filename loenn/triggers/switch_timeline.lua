local SwitchTimelineTrigger = {}


SwitchTimelineTrigger.name = "Time_Switch/SwitchTimelineTrigger"

SwitchTimelineTrigger.placements = {
    name = "Switch_Timeline",
    data = {
		Activate = 1,
		onlyOnce = true
    }
}

SwitchTimelineTrigger.fieldOrder = {
	"x",
	"y",
	"width",
	"height",
	"Activate",
	"onlyOnce"
}

SwitchTimelineTrigger.fieldInformation = { 
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


return SwitchTimelineTrigger