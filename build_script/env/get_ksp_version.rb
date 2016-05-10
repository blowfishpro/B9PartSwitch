#!/usr/bin/env ruby

require 'json'

version_info = JSON.parse(File.read('GameData/B9PartSwitch/B9PartSwitch.version'))
ksp_version = version_info['KSP_VERSION']

puts "#{ksp_version['MAJOR']}.#{ksp_version['MINOR']}.#{ksp_version['PATCH']}"