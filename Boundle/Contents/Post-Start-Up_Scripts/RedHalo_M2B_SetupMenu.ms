-- setup RedHalo M2B menu, and add our app to it.
(
    local stringRedHaloM2BMenu = "RedHalo Studio"

    -- so this should ensure it is only one time only per user UI setup
    -- MUST keep the value the SAME for all apps
    if menuMan.registerMenuContext 0x63e654af then
    (
        -- can take these out, but it shows that it is called only when needed...
        -- print "Creating RedHalo M2B menu"

        -- Get the main menu bar
        local mainMenuBar = menuMan.getMainMenuBar()

        -- Create the menu
        local subMenu = menuMan.createMenu stringRedHaloM2BMenu
        local menuPos = mainMenuBar.numItems()
        local submenuItem = menuMan.createSubMenuItem "test" subMenu

        mainMenuBar.addItem submenuItem menuPos
        -- redraw the menu bar with the new item
        menuMan.updateMenuBar()
    )

    -- This one will allow an app to add theirs to existing RedHalo M2B app.
    -- MUST use genclassid() to get this value and it should be DIFFERENT per app.
    -- This then allows only onetime setup per app, per user UI setup

    local stringMacroScriptName = "RedHaloM2B" 
    local stringMacroScriptCategory = "REDHALO STUDIO"
    if menuMan.registerMenuContext 0x38996e72 then
    (

        -- this should always succeed because above
        local appStoreMenu = menuMan.findMenu stringRedHaloM2BMenu

        -- create the action item    
        local actionItem1 = menuMan.createActionItem stringMacroScriptName stringMacroScriptCategory

        -- Add the action item as a menu entry
        appStoreMenu.addItem actionItem1 -1

        -- redraw the menu bar with the new item
        menuMan.updateMenuBar()
    )
)