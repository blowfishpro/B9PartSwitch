require_relative 'version'

def get_tag_version
	version_info = /[A-z]?(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+))?(-(?<build>\d+))?/.match `git describe --tags`

	version_major = version_info['major'].to_i
	version_minor = version_info['minor'].to_i
	version_patch = version_info['patch'].to_i
	version_build = version_info['build'].to_i

	Version.new(version_major, version_minor, version_patch, version_build)
end
