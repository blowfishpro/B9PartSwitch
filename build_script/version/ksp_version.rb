require_relative 'version'

def ksp_version
  version_raw = File.read('KSP_VERSION')
  version_info = /(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+))?/.match version_raw

  version_major = version_info[:major].to_i
  version_minor = version_info[:minor].to_i
  version_patch = version_info[:patch].to_i

  Version.new(version_major, version_minor, version_patch)
end
