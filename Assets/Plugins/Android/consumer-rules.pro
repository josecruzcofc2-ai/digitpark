# Consumer ProGuard rules for Skillz SDK
# These rules are applied to consuming projects

# Keep Skillz SDK classes
-keep class com.skillz.** { *; }
-dontwarn com.skillz.**

# Keep Unity classes
-keep class com.unity3d.** { *; }
-dontwarn com.unity3d.**
