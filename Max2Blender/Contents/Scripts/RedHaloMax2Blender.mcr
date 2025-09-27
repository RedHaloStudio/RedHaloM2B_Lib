macroScript RedHaloMax2Blender 
category:"REDHALO STUDIO" 
tooltip:"Export to Blender..." 
buttonText:"Export to Blender"
(    
    -- Execute the ADN Explode Geometry plugin; from a amanged CuiCommandAdaptor implemented plugin,
    -- you can grab the unqiue ID from the MAXScript listener window.
    actionMan.executeAction 59289 "63298"  -- REDHALO STUDIO: Export to Blender...
)
