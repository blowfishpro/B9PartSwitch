#!/usr/bin/env ruby

require 'json'

require_relative '../version/tag_version'
require_relative '../version/ksp_version'

version = get_tag_version
ksp_version = get_ksp_version

project_name = ENV['PROJECT_NAME']

avc_info = eval File.read('files/AVC.version.in')

File.open("GameData/#{project_name}/#{project_name}.version", 'w+') do |f|
  f.write JSON.pretty_generate(avc_info)
end
