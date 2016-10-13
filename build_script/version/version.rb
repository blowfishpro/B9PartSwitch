class Version
  attr_reader :major, :minor, :patch, :build
  
  def initialize(major=nil, minor=nil, patch=nil, build=nil)
    @major = major
    @minor = minor
    @patch = patch
    @build = build
  end
  
  def <=> (other)
    result = major <=> other.major
    return 0 if result.nil?
    return result unless result.zero?

    result = minor <=> other.minor
    return 0 if result.nil?
    return result unless result.zero?

    result = patch <=> other.patch
    return 0 if result.nil?
    return result unless result.zero?

    result = build <=> other.build
    return 0 if result.nil?
    return result unless result.zero?
    
    return 0 # All versions match
  end
  
  def == (other)
    (self <=> other) == 0
  end
  
  def to_s
    [].tap do |arr|
      [major, minor, patch, build].each do |n|
        break if n.nil?
        arr << n
      end
    end.join('.')
  end

  def full_version
    [major, minor, patch, build].map(&:to_i).join('.')
  end
end
