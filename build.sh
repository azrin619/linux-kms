cat << 'EOF' > build.sh
#!/bin/bash
BASE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export PATH="$BASE_DIR/toolchain/bin:$PATH"
export MONO_PATH="$BASE_DIR/toolchain/lib/mono/4.5:$BASE_DIR/toolchain/lib/mono/gac"
export MSBuildExtensionsPath="$BASE_DIR/toolchain/lib/mono/xbuild"

echo "Building KMS Server using bundled offline xbuild..."
xbuild /p:Configuration=Release "$BASE_DIR/src/KmsServer.csproj"

if [ -f "$BASE_DIR/src/bin/Release/KmsServer.exe" ]; then
    echo "Build Successful: $BASE_DIR/src/bin/Release/KmsServer.exe"
else
    echo "Build Failed."
    exit 1
fi
EOF

chmod +x build.sh

