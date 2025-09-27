(
    local strMenuTitle = "RedHaloTools"
    local strMacroScriptName = "RedHaloMax2Blender"
    local strMacroScriptCategory = "REDHALO STUDIO"

    local mainMenuBar = menuMan.getMainMenuBar()
	local MainMenuIdx = mainMenuBar.numItems() - 1

    local MainMenu = menuMan.findMenu strMenuTitle

	if MainMenu == undefined do (
        
        local mainSubMenu = menuMan.createMenu strMenuTitle

        actionItem = menuMan.createActionItem strMacroScriptName strMacroScriptCategory
        
        mainSubMenu.addItem actionItem -1

        -- Create a new menu item with the menu as it's sub-menu
        local subMenuItem = menuMan.createSubMenuItem "RHMenuName" mainSubMenu
            
        -- compute the index of the next-to-last menu item in the main menu bar
        local subMenuIndex = mainMenuBar.numItems() - 1
            
        -- Add the sub-menu just at the second to last slot
        mainMenuBar.addItem subMenuItem MainMenuIdx
        
        -- redraw the menu bar with the new item
        menuMan.updateMenuBar()
    )
)