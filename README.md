# FallingSand_CS

Yet another falling sand simulation I've made. This time, in C#.

This code is built upon https://github.com/rudyon/kum (mostly because I'm tired of writing the initial boilerplate for falling sand simulations and wanted something that already had that set up)

My changes being extensive enough (chunking, dirty rect optimization,different rendering,etc) to warrant a separated repository.

# Reasons for making this
I've made a lot of falling sand simulations, from Lua, to Beef, to Rust, and now C#. I mostly use them as my "beginner" project when learning a new language (apart from Lua and C#, which I was already knowledgable)

I chose C# for this mostly due to C# being a much more developed language than Beef in terms of libraries available for it and community-based support. Rust was a nice language, but I wanted something I was more familiar in for this project as I'm trying to make this as optimized as possible and I also found Lua/Love2D a bit too annoying to write a falling sand simulation in.

The plan for this is to make a somewhat indepth falling sand simulation that supports:

- [x] Chunks
- [x] Dirty rects
- [ ] Multithreading
- [ ] Fast particle movement (particles can move more than 1 cell per frame)
- [ ] Expandable/infinite world 
