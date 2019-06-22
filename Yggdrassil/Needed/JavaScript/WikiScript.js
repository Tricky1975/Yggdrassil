// This script just needs to be attached to every Wiki Page

const Var2Script = {}
const Var2ScriptCall = (vcall) => {
	if (!Var2Script[vcall]) {
		alert(`There is no script for Wiki Variable ${vcall}!\n\nPlease notify the webmaster of this error!`)
		return Undefined
	}
	return Var2Script[vcall](vcall)
}
