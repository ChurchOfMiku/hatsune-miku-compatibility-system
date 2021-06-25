
function isstring(x)
    return type(x) == "string"
end

function isfunction(x)
    return type(x) == "function"
end

TYPE_NONE = -1	-- Invalid type
TYPE_INVALID = -1
TYPE_NIL = 0	-- nil
TYPE_BOOL = 1	-- boolean
TYPE_LIGHTUSERDATA = 2 -- light userdata
TYPE_NUMBER = 3	-- number
TYPE_STRING = 4	-- string
TYPE_TABLE = 5	-- table
TYPE_FUNCTION = 6 -- function
TYPE_USERDATA = 7 -- userdata
TYPE_THREAD = 8 -- thread
TYPE_ENTITY = 9	-- Entity and entity sub-classes including Player, Weapon, NPC, Vehicle, CSEnt, and NextBot
TYPE_VECTOR = 10 -- Vector
TYPE_ANGLE = 11 -- Angle
TYPE_PHYSOBJ = 12 -- PhysObj
TYPE_SAVE = 13 -- ISave
TYPE_RESTORE = 14 -- IRestore
TYPE_DAMAGEINFO = 15 -- CTakeDamageInfo
TYPE_EFFECTDATA = 16 -- CEffectData
TYPE_MOVEDATA = 17 -- CMoveData
TYPE_RECIPIENTFILTER = 18 -- CRecipientFilter
TYPE_USERCMD = 19 -- CUserCmd
TYPE_SCRIPTEDVEHICLE = 20
TYPE_MATERIAL = 21 -- IMaterial
TYPE_PANEL = 22 -- Panel
TYPE_PARTICLE = 23 -- CLuaParticle
TYPE_PARTICLEEMITTER = 24 -- CLuaEmitter
TYPE_TEXTURE = 25 -- ITexture
TYPE_USERMSG = 26 -- bf_read
TYPE_CONVAR = 27 -- ConVar
TYPE_IMESH = 28 -- IMesh
TYPE_MATRIX = 29 -- VMatrix
TYPE_SOUND = 30 -- CSoundPatch
TYPE_PIXELVISHANDLE = 31 -- pixelvis_handle_t
TYPE_DLIGHT = 32 -- dlight_t. Metatable of a Structures/DynamicLight
TYPE_VIDEO = 33 -- IVideoWriter
TYPE_FILE = 34 -- File
TYPE_LOCOMOTION = 35 -- CLuaLocomotion
TYPE_PATH = 36 -- PathFollower
TYPE_NAVAREA = 37 -- CNavArea
TYPE_SOUNDHANDLE = 38 -- IGModAudioChannel
TYPE_NAVLADDER = 39 -- CNavLadder
TYPE_PARTICLESYSTEM = 40 -- CNewParticleEffect
TYPE_PROJECTEDTEXTURE = 41 -- ProjectedTexture
TYPE_PHYSCOLLIDE = 42 -- PhysCollide
TYPE_SURFACEINFO = 43 -- SurfaceInfo
TYPE_COUNT = 44	-- Amount of TYPE_* enums
TYPE_COLOR = 255
