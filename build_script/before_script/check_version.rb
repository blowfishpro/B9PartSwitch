#!/usr/bin/env ruby

require 'json'

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
end

avc_info = JSON.parse(File.read('GameData/B9PartSwitch/B9PartSwitch.version'))['VERSION']
avc_version = Version.new(avc_info['MAJOR'], avc_info['MINOR'], avc_info['PATCH'], avc_info['BUILD'])

assembly_info = File.read('B9PartSwitch/Properties/AssemblyInfo.cs')

assembly_version_info = assembly_info.match(/^\[assembly: AssemblyVersion\("(?<MAJOR>\d+)\.(?<MINOR>\d+)\.(?<PATCH>\d+)\.(?<BUILD>\d+)"\)\]$/)
assembly_file_version_info = assembly_info.match(/^\[assembly: AssemblyFileVersion\("(?<MAJOR>\d+)\.(?<MINOR>\d+)\.(?<PATCH>\d+)\.(?<BUILD>\d+)"\)\]$/)
ksp_assembly_version_info = assembly_info.match(/^\[assembly: KSPAssembly\("\w+", ?(?<MAJOR>\d+), ?(?<MINOR>\d+)\)\]$/)


assembly_version = Version.new(assembly_version_info['MAJOR'], assembly_version_info['MINOR'], assembly_version_info['PATCH'], assembly_version_info['BUILD'])
assembly_file_version = Version.new(assembly_file_version_info['MAJOR'], assembly_file_version_info['MINOR'], assembly_file_version_info['PATCH'], assembly_file_version_info['BUILD'])
ksp_assembly_version = Version.new(ksp_assembly_version_info['MAJOR'], ksp_assembly_version_info['MINOR'])

result = [assembly_version, assembly_file_version, ksp_assembly_version].all? { |v| v == avc_version }

unless result
  puts 'Not all versions equal'
  puts "AVC Version: #{avc_version}"
  puts "Assembly Version: #{assembly_version}"
  puts "Assembly File Version: #{assembly_file_version}"
  puts "KSP Assembly Version: #{ksp_assembly_version}"
  
  raise 'Not all versions equal'
end

puts "Version consistency check passed!"