class Version
  attr_reader :major, :minor, :patch, :build

  def initialize(major = nil, minor = nil, patch = nil, build = nil)
    @major = major
    @minor = minor
    @patch = patch
    @build = build
  end

  def <=>(other)
    [
      major <=> other.major,
      minor <=> other.minor,
      patch <=> other.patch,
      build <=> other.build,
    ].each do |result|
      case result
      when 0
        next
      when nil
        return 0
      else
        return result
      end
    end

    0 # All versions match
  end

  def ==(other)
    (self <=> other).zero?
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
